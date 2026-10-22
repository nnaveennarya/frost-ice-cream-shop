<?php

declare(strict_types=1);

namespace Database\Seeders;

use App\Models\Category;
use App\Models\Customer;
use App\Models\Item;
use Illuminate\Database\Seeder;

final class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        foreach (['Chocolate', 'Vanilla', 'Strawberry'] as $name) {
            $category = Category::firstOrCreate(['name' => $name]);
            if ($this->demoAllowed()) {
                Item::firstOrCreate(['name' => $name.' Classic', 'category_id' => $category->id], ['description' => 'Synthetic assessment inventory', 'price' => '275.00', 'quantity' => 25]);
            }
        }
        if ($this->demoAllowed()) {
            Customer::firstOrCreate(['name' => 'Sample Customer'], ['email' => 'sample@example.test']);
        }
    }

    private function demoAllowed(): bool
    {
        return app()->environment(['local', 'testing']) && in_array(config('database.connections.mysql.database'), ['ice_cream_assessment', 'ice_cream_assessment_test', 'ice_cream_assessment_rehearsal'], true);
    }
}
