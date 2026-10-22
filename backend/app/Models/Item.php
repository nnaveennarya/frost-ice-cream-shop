<?php

declare(strict_types=1);

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

final class Item extends Model
{
    protected $fillable = ['name', 'description', 'category_id', 'price', 'quantity'];

    protected function casts(): array
    {
        return ['price' => 'decimal:2', 'quantity' => 'integer', 'version' => 'integer'];
    }

    /** @return BelongsTo<Category, $this> */
    public function category(): BelongsTo
    {
        return $this->belongsTo(Category::class);
    }
}
