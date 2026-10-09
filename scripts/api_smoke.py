"""Run HTTP/JSON checks against the actual ASP.NET Core API (Python 3)."""
import argparse
import hashlib
import json
import time
from pathlib import Path
from urllib.error import HTTPError
from urllib.request import Request, urlopen

parser = argparse.ArgumentParser()
parser.add_argument("--base-url", default="http://127.0.0.1:5080")
parser.add_argument("--output", default="artifacts/api-smoke.json")
args = parser.parse_args()
transcript = []


def call(method, path, expected, body=None, raw=None, content_type="application/json"):
    data = raw if raw is not None else (
        json.dumps(body).encode() if body is not None else None)
    request = Request(args.base_url.rstrip("/") + path, data=data, method=method,
                      headers={"Content-Type": content_type})
    try:
        response = urlopen(request, timeout=10)
    except HTTPError as error:
        response = error
    status = response.status if hasattr(response, "status") else response.code
    with response:
        payload = json.loads(response.read())
        headers = dict(response.headers)
    assert status == expected, (method, path, status, payload)
    assert headers.get("Content-Type", "").startswith("application/json"), headers
    transcript.append({"Method": method, "Path": path, "Request": body,
                       "Status": status, "Response": payload})
    print(f"PASS: {method} {path} -> {status}")
    return payload, headers


suffix = str(time.time_ns())
login = "student_" + suffix
pass_hash = hashlib.sha256(b"lab02-demo-password").hexdigest()
request_body = {"Login": login, "PassHash": pass_hash}
created, headers = call("POST", "/user", 201, request_body)
user_id = created["Id"]
assert headers["Location"] == f"/user/{user_id}"
assert "PassHash" not in created
read, _ = call("GET", f"/user/{user_id}", 200)
assert read == {"Id": user_id, "Login": login}
call("POST", "/user", 409, request_body)
call("POST", "/user", 409, {**request_body, "Login": login.upper()})
call("POST", "/user", 400, {"Login": "valid_login", "PassHash": "plaintext"})
call("POST", "/user", 400, {"Login": "x", "PassHash": pass_hash})
call("POST", "/user", 400, {"Login": "valid_login"})
call("POST", "/user", 400, raw=b"{broken")
call("POST", "/user", 415, raw=b"text", content_type="text/plain")
call("GET", "/user/0", 400)
second, _ = call("POST", "/user", 201,
                 {"Login": "other_" + suffix, "PassHash": pass_hash})
call("PUT", f"/user/{user_id}", 409,
     {"Login": second["Login"], "PassHash": pass_hash})
new_body = {"Login": login + "_new", "PassHash": hashlib.sha256(b"changed").hexdigest()}
updated, _ = call("PUT", f"/user/{user_id}", 200, new_body)
assert updated["Login"] == new_body["Login"]
read, _ = call("GET", f"/user/{user_id}", 200)
assert read == updated
call("DELETE", f"/user/{user_id}", 200)
call("GET", f"/user/{user_id}", 404)
call("PUT", f"/user/{user_id}", 404, new_body)
call("DELETE", f"/user/{user_id}", 404)
call("DELETE", f"/user/{second['Id']}", 200)
call("GET", "/missing-route", 404)
output = Path(args.output)
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(transcript, ensure_ascii=False, indent=2), encoding="utf-8")
print(f"HTTP CHECKS PASSED: {len(transcript)}")
