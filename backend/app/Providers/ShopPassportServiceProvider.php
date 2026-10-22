<?php

declare(strict_types=1);

namespace App\Providers;

use App\Services\Auth\NoRefreshTokenRepository;
use Laravel\Passport\Bridge\RefreshTokenRepository;
use Laravel\Passport\Passport;
use Laravel\Passport\PassportServiceProvider;
use League\OAuth2\Server\AuthorizationServer;

final class ShopPassportServiceProvider extends PassportServiceProvider
{
    protected function registerAuthorizationServer(): void
    {
        // Reauthorization mode disables refresh acceptance and credential issuance.
        $this->app->bind(RefreshTokenRepository::class, NoRefreshTokenRepository::class);
        $this->app->singleton(AuthorizationServer::class, function (): AuthorizationServer {
            $server = $this->makeAuthorizationServer();
            $server->enableGrantType($this->makeAuthCodeGrant(), Passport::tokensExpireIn());

            return $server;
        });
    }
}
