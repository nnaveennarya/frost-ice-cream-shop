<?php

declare(strict_types=1);

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

final class ValidateOAuthInput
{
    private const PARAMETERS = [
        'client_id', 'redirect_uri', 'response_type', 'scope', 'state',
        'code_challenge', 'code_challenge_method', 'prompt', 'auth_token',
        'grant_type', 'code', 'code_verifier', 'refresh_token', 'client_secret',
        'username', 'password',
    ];

    public function handle(Request $request, Closure $next): Response
    {
        foreach (self::PARAMETERS as $parameter) {
            $value = $request->input($parameter);
            if ($value !== null && ! is_string($value)) {
                return response()->json(['error' => 'invalid_request', 'error_description' => 'OAuth parameters must be strings.'], 400);
            }
        }

        return $next($request);
    }
}
