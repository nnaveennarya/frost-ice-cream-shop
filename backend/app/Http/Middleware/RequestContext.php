<?php

declare(strict_types=1);

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Str;
use Symfony\Component\HttpFoundation\Response;

final class RequestContext
{
    public function handle(Request $request, Closure $next): Response
    {
        $request->attributes->set('request_id', (string) Str::uuid());
        if ($request->is('api/*') && ! $request->is('api/auth/login')) {
            abort_if(strlen($request->getContent()) > 65536, 413, 'Request exceeds 64 KiB.');
            abort_unless($request->wantsJson(), 406, 'Accept application/json is required.');
            if (in_array($request->method(), ['POST', 'PATCH'], true)) {
                abort_unless($request->isJson(), 415, 'Content-Type application/json is required.');
            }
        }
        $response = $next($request);
        $response->headers->set('X-Request-ID', $request->attributes->get('request_id'));
        $response->headers->set('X-Content-Type-Options', 'nosniff');
        $response->headers->set('Referrer-Policy', 'no-referrer');
        $response->headers->set('Cache-Control', 'no-store');
        if (! $request->is('api/*') && ! $request->is('oauth/token')) {
            $response->headers->set('Content-Security-Policy', "default-src 'self'; style-src 'self' 'unsafe-inline'; form-action 'self'; frame-ancestors 'none'; base-uri 'none'");
        }

        return $response;
    }
}
