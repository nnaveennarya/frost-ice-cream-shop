<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\Item;
use App\Models\User;

final class ItemPolicy
{
    public function viewAny(User $u): bool
    {
        return $u->is_active;
    }

    public function view(User $u, Item $item): bool
    {
        return $u->is_active;
    }

    public function create(User $u): bool
    {
        return $u->is_active && in_array($u->role, [UserRole::Owner, UserRole::Manager], true);
    }

    public function update(User $u, Item $item): bool
    {
        return $u->is_active;
    }

    public function delete(User $u, Item $item): bool
    {
        return $u->is_active && $u->role === UserRole::Owner;
    }
}
