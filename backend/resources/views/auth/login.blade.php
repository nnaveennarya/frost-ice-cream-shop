@extends('auth.layout')
@section('title','Sign in')
@section('content')
<h1>Choose your Frost account.</h1><p class="muted">Enter the email and password for the account you want to use in this app window. You can use a different account in another Frost window.</p>
@if(!empty($accounts))<p class="muted">Accounts previously used in this browser:</p>
<nav class="account-options" aria-label="Choose a previously used Frost account">
@foreach($accounts as $account)<a href="{{ route('login', ['email' => $account['email']]) }}">{{ $account['name'] }} <span>{{ $account['email'] }}</span></a>@endforeach
</nav>@endif
@if(session('status'))<p class="notice" role="status">{{ session('status') }}</p>@endif
@if($errors->any())<div class="notice error" role="alert" id="login-errors">{{ $errors->first() }}</div>@endif
<form method="post" action="{{ route('login.submit') }}">@csrf
<label for="email">Email address</label><input id="email" type="email" name="email" autocomplete="username" maxlength="254" required value="{{ old('email', request('email')) }}" @if($errors->any()) aria-describedby="login-errors" aria-invalid="true" @endif>
<label for="password">Password</label><input id="password" type="password" name="password" autocomplete="current-password" maxlength="1024" required>
<button type="submit">Sign in securely</button></form>
@endsection
