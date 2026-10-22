<?php

declare(strict_types=1);

namespace App\Actions;

use App\Exceptions\ApiProblem;
use App\Models\User;
use App\Services\Audit\AuditWriter;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\DB;

final class WriteResource
{
    public function __construct(private readonly AuditWriter $audit) {}

    public function create(Model $model, array $data, User $actor): Model
    {
        return DB::transaction(function () use ($model, $data, $actor): Model {
            $model->fill($data)->save();
            $this->audit->record($actor, 'create', $model, array_keys($data));

            return $model->refresh();
        });
    }

    public function update(Model $model, array $data, User $actor): Model
    {
        return DB::transaction(function () use ($model, $data, $actor): Model {
            $version = (int) $data['version'];
            unset($data['version']);
            $changed = $model->newQuery()->whereKey($model->getKey())->where('version', $version)
                ->update([...$data, 'version' => DB::raw('version + 1')]);
            $this->assertChanged($changed, $model);
            $this->audit->record($actor, 'update', $model, array_keys($data));

            return $model->refresh();
        });
    }

    public function delete(Model $model, int $version, User $actor): void
    {
        DB::transaction(function () use ($model, $version, $actor): void {
            $changed = $model->newQuery()->whereKey($model->getKey())->where('version', $version)->delete();
            $this->assertChanged($changed, $model);
            $this->audit->record($actor, 'delete', $model, []);
        });
    }

    private function assertChanged(int $changed, Model $model): void
    {
        if ($changed === 0) {
            if (! $model->newQuery()->whereKey($model->getKey())->exists()) {
                throw new ApiProblem(404, 'NOT_FOUND', 'Record not found.');
            }
            throw new ApiProblem(409, 'VERSION_CONFLICT', 'This record changed. Reload it before saving.');
        }
    }
}
