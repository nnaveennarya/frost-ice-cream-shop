<?php

declare(strict_types=1);

namespace App\Console\Commands;

use App\Enums\UserRole;
use App\Models\User;
use Illuminate\Console\Command;
use Illuminate\Support\Facades\Hash;
use Laravel\Passport\Client;
use Laravel\Passport\ClientRepository;

final class SetupAssessment extends Command
{
    protected $signature = 'shop:setup {--password-file= : Private JSON containing owner, manager, cashier passwords}';

    protected $description = 'Create local assessment users and two public PKCE clients without overwriting passwords';

    public function handle(ClientRepository $clients): int
    {
        if (! app()->environment(['local', 'testing']) || ! in_array(config('database.connections.mysql.database'), ['ice_cream_assessment', 'ice_cream_assessment_test', 'ice_cream_assessment_rehearsal'], true)) {
            $this->error('Demo setup requires local/testing and a named disposable assessment database.');

            return self::FAILURE;
        }
        $file = $this->option('password-file');
        if (! is_string($file) || ! is_file($file)) {
            $this->error('Supply a private password JSON file.');

            return self::FAILURE;
        }
        $passwords = json_decode((string) file_get_contents($file), true, 512, JSON_THROW_ON_ERROR);
        foreach (UserRole::cases() as $role) {
            $password = $passwords[$role->value] ?? null;
            if (! is_string($password) || strlen($password) < 16 || strlen($password) > 1024) {
                $this->error('Each role requires a password of 16–1024 bytes.');

                return self::FAILURE;
            }
        }
        foreach (UserRole::cases() as $role) {
            User::firstOrCreate(['email' => $role->value.'@icecream.test'], ['name' => ucfirst($role->value), 'role' => $role, 'is_active' => true, 'password' => Hash::make($passwords[$role->value])]);
        }
        $this->call('db:seed', ['--force' => true, '--no-interaction' => true]);
        foreach (['WPF' => 'http://127.0.0.1:43821/callback/', 'Postman' => 'https://oauth.pstmn.io/v1/browser-callback'] as $name => $redirect) {
            $client = Client::query()->where('name', 'Ice Cream Shop '.$name)->where('revoked', false)->first();
            if (! $client) {
                $client = $clients->createAuthorizationCodeGrantClient('Ice Cream Shop '.$name, [$redirect], false);
            }
            $client->forceFill(['grant_types' => ['authorization_code']])->save();
            $this->line($name.' public client ID: '.$client->id);
        }
        $this->info('Local accounts ready. Existing passwords were preserved.');

        return self::SUCCESS;
    }
}
