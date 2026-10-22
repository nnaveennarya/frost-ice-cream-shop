<?php

declare(strict_types=1);

namespace App\Http\Requests\Items;

use App\Http\Requests\ShopWriteRequest;
use App\Models\Item;

final class WriteItemRequest extends ShopWriteRequest
{
    protected function resourceClass(): string
    {
        return Item::class;
    }

    protected function resourceParameter(): string
    {
        return 'item';
    }

    public function rules(): array
    {
        $p = $this->presence();

        return [
            'name' => [$p, 'required', 'string', 'max:120'],
            'description' => ['sometimes', 'nullable', 'string', 'max:1000'],
            'category_id' => [$p, 'required', 'integer', 'exists:categories,id'],
            'price' => [$p, 'required', 'regex:/^\\d{1,8}(\\.\\d{1,2})?$/D', 'numeric', 'min:0', 'max:99999999.99'],
            'quantity' => [$p, 'required', 'integer', 'min:0', 'max:4294967295'],
            ...($this->isMethod('PATCH') ? ['version' => ['required', 'integer', 'min:1', 'max:4294967295']] : []),
        ];
    }
}
