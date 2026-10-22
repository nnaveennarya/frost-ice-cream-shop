<?php

declare(strict_types=1);

namespace App\Providers;

use Illuminate\Cache\RateLimiting\Limit;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\RateLimiter;
use Illuminate\Support\ServiceProvider;
use Laravel\Passport\Passport;

final class AppServiceProvider extends ServiceProvider
{
    public function register(): void
    {
        Passport::ignoreRoutes();
    }

    public function boot(): void
    {
        Passport::loadKeysFrom(app()->environment('testing') ? storage_path('oauth-testing') : storage_path());
        Passport::tokensCan(['shop:access' => 'Access the ice cream shop']);
        Passport::tokensExpireIn(now()->addMinutes(15));
        Passport::authorizationView('auth.authorize');
        RateLimiter::for('login', function (Request $r): array {
            $email = $r->input('email');
            $identity = is_string($email) ? strtolower(trim($email)) : 'malformed-input';

            return [
                Limit::perMinute(5)->by(hash('sha256', $identity.'|'.$r->ip())),
                Limit::perMinute(30)->by((string) $r->ip()),
            ];
        });
        RateLimiter::for('token', fn (Request $r): Limit => Limit::perMinute(30)->by((string) $r->ip()));
        RateLimiter::for('api', fn (Request $r): Limit => Limit::perMinute(300)->by((string) $r->user()->id));
        RateLimiter::for('writes', fn (Request $r): Limit => Limit::perMinute(60)->by((string) $r->user()?->id));
    }
}
