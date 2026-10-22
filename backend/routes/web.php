<?php

declare(strict_types=1);
use App\Http\Controllers\Auth\BrowserLogoutController;
use App\Http\Controllers\Auth\LoginController;
use App\Http\Middleware\RequirePkce;
use App\Http\Middleware\RequireOAuthLogin;
use App\Http\Middleware\ValidateOAuthInput;
use App\Services\Auth\RememberedAccounts;
use Illuminate\Support\Facades\Route;
use Laravel\Passport\Http\Controllers\ApproveAuthorizationController;
use Laravel\Passport\Http\Controllers\AuthorizationController;
use Laravel\Passport\Http\Controllers\DenyAuthorizationController;

Route::redirect('/', '/login');
Route::get('/login', function (\Illuminate\Http\Request $request) {
    return view('auth.login', ['accounts' => RememberedAccounts::fromCookie($request->cookie('frost_accounts'))]);
})->name('login');
Route::post('/login', LoginController::class)->middleware('throttle:login')->name('login.submit');
Route::post('/api/auth/login', LoginController::class)->middleware('throttle:login');
Route::view('/auth/signed-in', 'auth.signed-in')->name('signed-in');
Route::view('/auth/sign-out', 'auth.sign-out');
Route::post('/auth/browser-logout', BrowserLogoutController::class);
Route::get('/oauth/authorize', [AuthorizationController::class, 'authorize'])->middleware([ValidateOAuthInput::class, RequirePkce::class, RequireOAuthLogin::class])->name('passport.authorizations.authorize');
Route::post('/oauth/authorize', [ApproveAuthorizationController::class, 'approve'])->middleware([ValidateOAuthInput::class, 'auth'])->name('passport.authorizations.approve');
Route::delete('/oauth/authorize', [DenyAuthorizationController::class, 'deny'])->middleware([ValidateOAuthInput::class, 'auth'])->name('passport.authorizations.deny');
