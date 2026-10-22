<?php

declare(strict_types=1);

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Customer extends Model
{
    protected $fillable = ['name', 'email', 'phone'];

    protected function casts(): array
    {
        return ['version' => 'integer'];
    }
}
