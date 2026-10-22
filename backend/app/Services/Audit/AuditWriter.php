<?php

declare(strict_types=1);

namespace App\Services\Audit;

use App\Models\AuditEvent;
use App\Models\User;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Str;

final class AuditWriter
{
    public function record(User $actor, string $action, Model $record, array $fields): void
    {
        AuditEvent::query()->create(['actor_user_id' => $actor->id, 'action' => $action, 'entity_type' => $record->getTable(),
            'entity_id' => $record->getKey(), 'request_id' => request()->attributes->get('request_id') ?? (string) Str::uuid(),
            'changed_fields' => array_values($fields)]);
    }
}
