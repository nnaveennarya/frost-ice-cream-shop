<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\WriteResource;
use App\Http\Requests\DeleteVersion;
use App\Http\Requests\Items\WriteItemRequest;
use App\Http\Requests\ListResourceRequest;
use App\Http\Resources\ItemResource;
use App\Models\Item;
use App\Queries\ResourceQuery;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Support\Facades\Gate;
use Symfony\Component\HttpFoundation\Response;

final class ItemController
{
    public function __construct(private readonly WriteResource $writer, private readonly ResourceQuery $query, private readonly DeleteVersion $versions) {}

    public function index(ListResourceRequest $request): AnonymousResourceCollection
    {
        Gate::authorize('viewAny', Item::class);

        return ItemResource::collection($this->query->paginate(Item::query(), $request->validated(), true));
    }

    public function show(Item $item): ItemResource
    {
        Gate::authorize('view', $item);

        return new ItemResource($item->load('category'));
    }

    public function store(WriteItemRequest $request): JsonResponse
    {
        $model = $this->writer->create(new Item, $request->validated(), $request->user());

        return (new ItemResource($model->load('category')))->response()->setStatusCode(201)->header('Location', route('items.show', $model->getKey()));
    }

    public function update(WriteItemRequest $request, Item $item): ItemResource
    {
        return new ItemResource($this->writer->update($item, $request->validated(), $request->user())->load('category'));
    }

    public function destroy(Request $request, Item $item): Response
    {
        Gate::authorize('delete', $item);
        $this->writer->delete($item, $this->versions->parse($request), $request->user());

        return response()->noContent();
    }
}
