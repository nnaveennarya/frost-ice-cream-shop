<?php

declare(strict_types=1);

namespace App\Services\Auth;

use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Cookie;

final class RememberedAccounts
{
    /** @return list<array{email: string, name: string}> */
    public static function fromCookie(mixed $remembered): array
    {
        if (! is_string($remembered)) {
            return [];
        }
        $decoded = json_decode($remembered, true);
        if (! is_array($decoded)) {
            return [];
        }

        return array_values(array_filter($decoded, static fn ($account): bool =>
            is_array($account) &&
            isset($account['email'], $account['name']) &&
            is_string($account['email']) &&
            is_string($account['name']) &&
            filter_var($account['email'], FILTER_VALIDATE_EMAIL) !== false
        ));
    }

    public function remember(Request $request, User $user): void
    {
        $accounts = array_values(array_filter(
            self::fromCookie($request->cookie('frost_accounts')),
            static fn (array $account): bool => $account['email'] !== $user->email,
        ));
        array_unshift($accounts, [
            'email' => $user->email,
            'name' => substr($user->name, 0, 80),
        ]);

        Cookie::queue(cookie(
            'frost_accounts',
            json_encode(array_slice($accounts, 0, 5), JSON_THROW_ON_ERROR),
            525600,
            '/',
            null,
            false,
            true,
            false,
            'lax',
        ));
    }
}
