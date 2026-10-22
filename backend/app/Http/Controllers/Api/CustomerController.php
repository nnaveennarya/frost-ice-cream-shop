<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\WriteResource;
use App\Http\Requests\Customers\WriteCustomerRequest;
use App\Http\Requests\DeleteVersion;
use App\Http\Requests\ListResourceRequest;
use App\Http\Resources\CustomerResource;
use App\Models\Customer;
use App\Queries\ResourceQuery;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Support\Facades\Gate;
use Symfony\Component\HttpFoundation\Response;

final class CustomerController
{
    public function __construct(private readonly WriteResource $writer, private readonly ResourceQuery $query, private readonly DeleteVersion $versions) {}

    public function index(ListResourceRequest $request): AnonymousResourceCollection
    {
        Gate::authorize('viewAny', Customer::class);

        return CustomerResource::collection($this->query->paginate(Customer::query(), $request->validated(), false));
    }

    public function show(Customer $customer): CustomerResource
    {
        Gate::authorize('view', $customer);

        return new CustomerResource($customer);
    }

    public function store(WriteCustomerRequest $request): JsonResponse
    {
        $model = $this->writer->create(new Customer, $request->validated(), $request->user());

        return (new CustomerResource($model))->response()->setStatusCode(201)->header('Location', route('customers.show', $model->getKey()));
    }

    public function update(WriteCustomerRequest $request, Customer $customer): CustomerResource
    {
        return new CustomerResource($this->writer->update($customer, $request->validated(), $request->user()));
    }

    public function destroy(Request $request, Customer $customer): Response
    {
        Gate::authorize('delete', $customer);
        $this->writer->delete($customer, $this->versions->parse($request), $request->user());

        return response()->noContent();
    }
}
