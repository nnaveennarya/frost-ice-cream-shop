<?php

declare(strict_types=1);
use App\Exceptions\ApiProblem;
use App\Http\Middleware\RequestContext;
use App\Http\Middleware\ValidateOAuthInput;
use Illuminate\Auth\Access\AuthorizationException;
use Illuminate\Auth\AuthenticationException;
use Illuminate\Database\Eloquent\ModelNotFoundException;
use Illuminate\Foundation\Application;
use Illuminate\Foundation\Configuration\Exceptions;
use Illuminate\Foundation\Configuration\Middleware;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Facades\Route;
use Illuminate\Validation\ValidationException;
use Laravel\Passport\Http\Controllers\AccessTokenController;
use Symfony\Component\HttpKernel\Exception\HttpExceptionInterface;

return Application::configure(basePath: dirname(__DIR__))
    ->withRouting(web: __DIR__.'/../routes/web.php', api: __DIR__.'/../routes/api.php', commands: __DIR__.'/../routes/console.php', health: '/up', then: function (): void {
        Route::post('/oauth/token', [AccessTokenController::class, 'issueToken'])->middleware([ValidateOAuthInput::class, 'throttle:token'])->name('passport.token');
    })
    ->withMiddleware(function (Middleware $m): void {
        $m->prepend(RequestContext::class);
        $m->redirectGuestsTo('/login');
    })
    ->withExceptions(function (Exceptions $e): void {
        $e->report(function (Throwable $error): bool {
            Log::error('Request failed', ['exception_type' => get_class($error), 'request_id' => request()->attributes->get('request_id')]);

            return false;
        });
        $e->shouldRenderJsonWhen(fn (Request $r): bool => $r->is('api/*') || $r->expectsJson());
        $e->render(function (Throwable $error, Request $r) {
            if ($r->isMethod('POST') && $r->is('login')) {
                $status = $error instanceof HttpExceptionInterface ? $error->getStatusCode() : null;
                $errors = match (true) {
                    $error instanceof ValidationException => $error->errors(),
                    $status === 429 => ['login' => 'Too many sign-in attempts. Please wait a minute, then try again.'],
                    $status === 419 => ['login' => 'This sign-in page expired. Reload it and try again.'],
                    default => ['login' => 'Sign-in could not be completed. Check your details and try again.'],
                };

                return redirect()->route('login')->withInput($r->only('email'))->withErrors($errors);
            }
            if (! $r->is('api/*')) {
                return null;
            }
            $status = match (true) {
                $error instanceof ValidationException => 422,
                $error instanceof AuthenticationException => 401,
                $error instanceof AuthorizationException => 403,
                $error instanceof ModelNotFoundException => 404,
                $error instanceof HttpExceptionInterface => $error->getStatusCode(),
                default => 500,
            };
            $code = $error instanceof ApiProblem ? $error->errorCode : match ($status) {
                401 => 'UNAUTHENTICATED',403 => 'FORBIDDEN',404 => 'NOT_FOUND',409 => 'VERSION_CONFLICT',419 => 'CSRF_FAILED',
                422 => 'VALIDATION_FAILED',428 => 'PRECONDITION_REQUIRED',429 => 'RATE_LIMITED',413 => 'PAYLOAD_TOO_LARGE',
                405 => 'METHOD_NOT_ALLOWED',406 => 'NOT_ACCEPTABLE',415 => 'UNSUPPORTED_MEDIA_TYPE',default => 'SERVER_ERROR',
            };
            $message = match ($status) {
                401 => 'Authentication is required.',403 => 'You do not have permission for this action.',404 => 'The requested record was not found.',
                422 => 'The supplied fields are invalid.',429 => 'Too many requests. Try again later.',500 => 'An unexpected server error occurred.',
                default => $error->getMessage(),
            };
            $headers = $error instanceof HttpExceptionInterface ? $error->getHeaders() : [];

            return response()->json(['error' => ['code' => $code, 'message' => $message, 'fields' => $error instanceof ValidationException ? $error->errors() : (object) []], 'request_id' => $r->attributes->get('request_id')], $status, [...$headers, 'X-Request-ID' => (string) $r->attributes->get('request_id')]);
        });
    })->create();
