<?php

declare(strict_types=1);

namespace App\Queries;

use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;

final class ResourceQuery
{
    public function paginate(Builder $query, array $input, bool $items = false): LengthAwarePaginator
    {
        if ($items) {
            $query->with('category');
        }
        if (($input['search'] ?? '') !== '') {
            $term = str_replace(['!', '%', '_'], ['!!', '!%', '!_'], $input['search']);
            $query->whereRaw("name LIKE ? ESCAPE '!'", ['%'.$term.'%']);
        }
        if ($items && isset($input['category_id'])) {
            $query->where('category_id', $input['category_id']);
        }
        $sort = $input['sort'] ?? 'name';
        $column = ltrim($sort, '-');
        $query->orderBy($column, str_starts_with($sort, '-') ? 'desc' : 'asc');
        if ($column !== 'id') {
            $query->orderBy('id');
        }

        return $query->paginate($input['per_page'] ?? 20)->withQueryString();
    }
}
