<?php

declare(strict_types=1);

namespace App\Services\Auth;

use Laravel\Passport\Bridge\RefreshTokenRepository;
use League\OAuth2\Server\Entities\RefreshTokenEntityInterface;

final class NoRefreshTokenRepository extends RefreshTokenRepository
{
    public function getNewRefreshToken(): ?RefreshTokenEntityInterface
    {
        return null;
    }
}
