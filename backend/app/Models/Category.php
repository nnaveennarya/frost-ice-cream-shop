<?php

declare(strict_types=1);

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

final class Category extends Model
{
    protected $fillable = ['name'];
}
