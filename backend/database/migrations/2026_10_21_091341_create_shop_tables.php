<?php

declare(strict_types=1);
use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('categories', function (Blueprint $t): void {
            $t->id();
            $t->string('name', 80)->unique();
            $t->timestamps();
        });
        Schema::create('items', function (Blueprint $t): void {
            $t->id();
            $t->string('name', 120);
            $t->string('description', 1000)->nullable();
            $t->foreignId('category_id')->constrained()->restrictOnDelete();
            $t->decimal('price', 10, 2);
            $t->unsignedInteger('quantity');
            $t->unsignedInteger('version')->default(1);
            $t->timestamps();
            $t->index(['created_at', 'id']);
            $t->index(['name', 'id']);
        });
        DB::statement('ALTER TABLE items ADD CONSTRAINT items_price_nonnegative CHECK (price >= 0), ADD CONSTRAINT items_version_positive CHECK (version >= 1)');
        Schema::create('customers', function (Blueprint $t): void {
            $t->id();
            $t->string('name', 120);
            $t->string('email', 254)->nullable();
            $t->string('phone', 32)->nullable();
            $t->unsignedInteger('version')->default(1);
            $t->timestamps();
            $t->index(['created_at', 'id']);
            $t->index(['name', 'id']);
        });
        DB::statement('ALTER TABLE customers ADD CONSTRAINT customers_version_positive CHECK (version >= 1)');
        Schema::create('audit_events', function (Blueprint $t): void {
            $t->id();
            $t->foreignId('actor_user_id')->nullable()->constrained('users')->nullOnDelete();
            $t->string('action', 20);
            $t->string('entity_type', 30);
            $t->unsignedBigInteger('entity_id');
            $t->uuid('request_id');
            $t->json('changed_fields');
            $t->timestamp('created_at')->useCurrent();
            $t->index(['entity_type', 'entity_id']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('audit_events');
        Schema::dropIfExists('customers');
        Schema::dropIfExists('items');
        Schema::dropIfExists('categories');
    }
};
