<?php

declare(strict_types=1);

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

final class ListResourceRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return ['page' => ['sometimes', 'integer', 'min:1', 'max:1000000'], 'per_page' => ['sometimes', 'integer', 'min:1', 'max:100'],
            'sort' => ['sometimes', Rule::in(['name', '-name', 'created_at', '-created_at', 'id', '-id'])],
            'search' => ['sometimes', 'nullable', 'string', 'max:100'], 'category_id' => ['sometimes', 'integer', 'min:1', 'exists:categories,id']];
    }
}
