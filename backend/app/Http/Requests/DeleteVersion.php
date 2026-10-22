<?php

declare(strict_types=1);

namespace App\Http\Requests;

use App\Exceptions\ApiProblem;
use Illuminate\Http\Request;

final class DeleteVersion
{
    public function parse(Request $request): int
    {
        if ($request->all() !== []) {
            throw new ApiProblem(422, 'VALIDATION_FAILED', 'DELETE does not accept body or query fields.');
        }
        $header = $request->header('If-Match');
        if ($header === null) {
            throw new ApiProblem(428, 'PRECONDITION_REQUIRED', 'If-Match is required.');
        }
        if (! preg_match('/^"([1-9][0-9]{0,9})"$/D', $header, $matches) || (int) $matches[1] > 4294967295) {
            throw new ApiProblem(422, 'VALIDATION_FAILED', 'If-Match must contain a quoted positive integer version.');
        }

        return (int) $matches[1];
    }
}
