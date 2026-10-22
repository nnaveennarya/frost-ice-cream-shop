<?php

declare(strict_types=1);
use App\Http\Controllers\Api\CategoryController;
use App\Http\Controllers\Api\CustomerController;
use App\Http\Controllers\Api\ItemController;
use App\Http\Controllers\Api\LogoutController;
use App\Http\Controllers\Api\MeController;
use App\Http\Middleware\ShopAccess;
use Illuminate\Support\Facades\Route;

Route::pattern('item', '[1-9][0-9]*');
Route::pattern('customer', '[1-9][0-9]*');

Route::middleware(['auth:api', ShopAccess::class, 'throttle:api'])->group(function (): void {
    Route::get('/auth/me', MeController::class);
    Route::post('/auth/logout', LogoutController::class);
    Route::get('/categories', CategoryController::class);
    Route::apiResource('items', ItemController::class)->only(['index', 'show']);
    Route::apiResource('customers', CustomerController::class)->only(['index', 'show']);
    Route::middleware('throttle:writes')->group(function (): void {
        Route::apiResource('items', ItemController::class)->only(['store', 'destroy']);
        Route::patch('/items/{item}', [ItemController::class, 'update']);
        Route::apiResource('customers', CustomerController::class)->only(['store', 'destroy']);
        Route::patch('/customers/{customer}', [CustomerController::class, 'update']);
    });
});
