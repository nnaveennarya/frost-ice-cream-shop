<?php

declare(strict_types=1);

namespace App\Http\Controllers\Auth;

use App\Models\User;
use App\Services\Auth\RememberedAccounts;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Validation\ValidationException;

final class LoginController
{
    public function __invoke(Request $request, RememberedAccounts $rememberedAccounts): JsonResponse|RedirectResponse
    {
        $data = $request->validate(['email' => ['required', 'string', 'email', 'max:254'], 'password' => ['required', 'string', 'max:1024']]);
        $data['email'] = strtolower(trim($data['email']));
        if (! Auth::guard('web')->attempt([...$data, 'is_active' => true], false)) {
            throw ValidationException::withMessages(['email' => ['The supplied credentials could not be verified.']]);
        }
        $request->session()->regenerate();
        if ($request->expectsJson()) {
            return response()->json(['data' => ['authenticated' => true]]);
        }
        $user = Auth::guard('web')->user();
        if (! $user instanceof User) {
            throw new \LogicException('The authenticated Frost account is unavailable.');
        }
        $rememberedAccounts->remember($request, $user);
        $pending = $request->session()->pull('url.intended');
        $safe = is_string($pending) && str_starts_with($pending, url('/oauth/authorize').'?');

        return redirect()->to($safe ? $pending : route('signed-in'));
    }
}
