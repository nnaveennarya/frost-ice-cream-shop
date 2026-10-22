<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Models\Category;
use Illuminate\Http\JsonResponse;

final class CategoryController
{
    public function __invoke(): JsonResponse
    {
        return response()->json(['data' => Category::query()->orderBy('name')->get(['id', 'name'])]);
    }
}
