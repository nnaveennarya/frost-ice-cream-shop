<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Laravel\Passport\AccessToken;
use Symfony\Component\HttpFoundation\Response;

final class LogoutController
{
    public function __invoke(Request $request): Response
    {
        $token = $request->user()->currentAccessToken();
        abort_unless($token instanceof AccessToken, 401);
        DB::transaction(function () use ($token): void {
            $token->revoke();
            DB::table('oauth_refresh_tokens')->where('access_token_id', $token->id)->update(['revoked' => true]);
        });

        return response()->noContent();
    }
}
