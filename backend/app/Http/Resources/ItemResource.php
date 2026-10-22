<?php

declare(strict_types=1);

namespace App\Http\Resources;

use App\Models\Item;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin Item */
final class ItemResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return ['id' => $this->id, 'name' => $this->name, 'description' => $this->description,
            'category_id' => $this->category_id, 'category' => ['id' => $this->category->id, 'name' => $this->category->name],
            'price' => $this->price, 'quantity' => $this->quantity, 'version' => $this->version,
            'created_at' => $this->created_at->toISOString(), 'updated_at' => $this->updated_at->toISOString()];
    }
}
