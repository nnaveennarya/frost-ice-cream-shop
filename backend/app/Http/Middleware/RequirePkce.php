<?php

declare(strict_types=1);

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Laravel\Passport\Client;
use Symfony\Component\HttpFoundation\Response;

final class RequirePkce
{
    public function handle(Request $request, Closure $next): Response
    {
        $clientId = $request->query('client_id');
        $redirectUri = $request->query('redirect_uri');
        if (! is_string($clientId) || ! is_string($redirectUri)) {
            return response()->json(['error' => 'invalid_request', 'error_description' => 'A client and exact redirect URI are required.'], 400);
        }
        $client = Client::query()->find($clientId);
        if (! $client || $client->revoked || $client->confidential() || ! in_array($redirectUri, $client->getAttribute('redirect_uris'), true)) {
            return response()->json(['error' => 'invalid_request', 'error_description' => 'The client or exact redirect URI is invalid.'], 400);
        }
        $scope = $request->query('scope') ?? '';
        if (! is_string($scope)) {
            return response()->json(['error' => 'invalid_request', 'error_description' => 'The scope must be a string.'], 400);
        }
        $scopes = preg_split('/\s+/', trim($scope), -1, PREG_SPLIT_NO_EMPTY);
        if (array_diff($scopes, ['shop:access']) !== []) {
            return response()->json(['error' => 'invalid_scope', 'error_description' => 'Only shop:access is supported.'], 400);
        }
        $challenge = $request->query('code_challenge');
        $state = $request->query('state');
        if ($request->query('code_challenge_method') !== 'S256' ||
            ! is_string($challenge) || ! is_string($state) ||
            ! preg_match('/^[A-Za-z0-9_-]{43}$/D', $challenge) ||
            ! preg_match('/^[A-Za-z0-9_-]{32,256}$/D', $state)) {
            return response()->json(['error' => 'invalid_request', 'error_description' => 'A S256 PKCE challenge and unpredictable state are required.'], 400);
        }
        if ($request->user() && ! $request->user()->is_active) {
            abort(403, 'Account unavailable.');
        }

        return $next($request);
    }
}
