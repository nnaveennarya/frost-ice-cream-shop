<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\Customer;
use App\Models\User;

final class CustomerPolicy
{
    public function viewAny(User $u): bool
    {
        return $u->is_active;
    }

    public function view(User $u, Customer $customer): bool
    {
        return $u->is_active;
    }

    public function create(User $u): bool
    {
        return $u->is_active;
    }

    public function update(User $u, Customer $customer): bool
    {
        return $u->is_active;
    }

    public function delete(User $u, Customer $customer): bool
    {
        return $u->is_active && $u->role === UserRole::Owner;
    }
}
