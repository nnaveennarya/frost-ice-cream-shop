<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Models\Customer;
use App\Models\Item;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

final class MeController
{
    public function __invoke(Request $request): JsonResponse
    {
        $u = $request->user();

        return response()->json(['data' => ['id' => $u->id, 'name' => $u->name, 'role' => $u->role->value, 'capabilities' => [
            'items.create' => $u->can('create', Item::class),
            'items.update' => $u->can('update', new Item),
            'items.delete' => $u->can('delete', new Item),
            'customers.create' => $u->can('create', Customer::class),
            'customers.update' => $u->can('update', new Customer),
            'customers.delete' => $u->can('delete', new Customer),
        ]]]);
    }
}
