<?php

declare(strict_types=1);

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Validator;

abstract class ShopWriteRequest extends FormRequest
{
    abstract public function rules(): array;

    abstract protected function resourceClass(): string;

    abstract protected function resourceParameter(): string;

    public function authorize(): bool
    {
        $model = $this->route($this->resourceParameter());

        return $this->user()?->can($model ? 'update' : 'create', $model ?? $this->resourceClass()) ?? false;
    }

    public function after(): array
    {
        return [function (Validator $v): void {
            $allowed = array_keys($this->rules());
            foreach (array_diff(array_keys($this->all()), $allowed) as $key) {
                $v->errors()->add($key, 'This field is not permitted.');
            }
            if ($this->isMethod('PATCH') && count(array_diff(array_keys($this->all()), ['version'])) === 0) {
                $v->errors()->add('body', 'Supply at least one field to update.');
            }
        }];
    }

    protected function presence(): string
    {
        return $this->isMethod('POST') ? 'required' : 'sometimes';
    }
}
