<?php

declare(strict_types=1);

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Symfony\Component\HttpFoundation\Response;

final class RequireOAuthLogin
{
    public function handle(Request $request, Closure $next): Response
    {
        if ($request->query('prompt') === 'login') {
            $freshState = $request->session()->pull('frost.oauth_login_state');
            $state = $request->query('state');
            if (is_string($freshState) && is_string($state) && hash_equals($freshState, $state)) {
                return $next($request);
            }

            Auth::guard('web')->logout();
            $request->session()->invalidate();
            $request->session()->regenerateToken();
            $request->session()->put('frost.oauth_login_state', (string) $request->query('state'));
        }

        return $next($request);
    }
}
