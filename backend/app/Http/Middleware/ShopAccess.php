<?php

declare(strict_types=1);

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

final class ShopAccess
{
    public function handle(Request $request, Closure $next): Response
    {
        $user = $request->user();
        abort_unless($user && $user->is_active, 403, 'Account unavailable.');
        abort_unless($user->tokenCan('shop:access'), 403, 'The shop:access scope is required.');

        return $next($request);
    }
}
