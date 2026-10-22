<?php

declare(strict_types=1);

namespace App\Http\Requests\Customers;

use App\Http\Requests\ShopWriteRequest;
use App\Models\Customer;

final class WriteCustomerRequest extends ShopWriteRequest
{
    protected function resourceClass(): string
    {
        return Customer::class;
    }

    protected function resourceParameter(): string
    {
        return 'customer';
    }

    public function rules(): array
    {
        return [
            'name' => [$this->presence(), 'required', 'string', 'max:120'],
            'email' => ['sometimes', 'nullable', 'string', 'email:rfc', 'max:254'],
            'phone' => ['sometimes', 'nullable', 'string', 'max:32'],
            ...($this->isMethod('PATCH') ? ['version' => ['required', 'integer', 'min:1', 'max:4294967295']] : []),
        ];
    }
}
