@extends('auth.layout')
@section('title','Authorize access')
@section('content')
<h1>Connect your shop.</h1><p><strong>{{ $client->name }}</strong> wants to connect as <strong>{{ $user->name }}</strong>.</p>
<p class="notice">The application can access shop inventory and customers within your account permissions. Access expires after 15 minutes.</p>
<p class="muted">Continue only if you started this sign-in from an application you trust.</p>
<form method="post" action="{{ route('passport.authorizations.approve') }}">@csrf
<input type="hidden" name="state" value="{{ $request->state }}"><input type="hidden" name="client_id" value="{{ $client->id }}"><input type="hidden" name="auth_token" value="{{ $authToken }}"><button>Authorize {{ $client->name }}</button></form>
<form method="post" action="{{ route('passport.authorizations.deny') }}">@csrf @method('DELETE')
<input type="hidden" name="state" value="{{ $request->state }}"><input type="hidden" name="client_id" value="{{ $client->id }}"><input type="hidden" name="auth_token" value="{{ $authToken }}"><button class="secondary">Cancel connection</button></form>
@endsection
