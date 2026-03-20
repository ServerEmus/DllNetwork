# Communication (broadcast) with APPNAME via Web
This communication exist for accounts that cannot use normal broadcast on the PC

## ErrorCodes
There are many errors whenever using this app can happen to break the software.\
1 -> Account already exists
2 -> Account does not exists

## Start
Your server must store this account until its calls "stop".\
The app automaticly tries to call the `/start` endpoint once `BroadcastCustom.Start()` called.\
The request:
```
POST /start

{
    "accountId": "1",
    "addresses":
    [
        "192.168.1.50",
        "192.168.3.50",
        "26.525.151.51",
        "::1"
    ],
    "port": 35666
}
```
Whenever the account already exist respond with: 
```
400

1
```

## Stop
Your server must remove this account from the storage.\
The app will call `/stop?accountId=<ID>` when  `BroadcastCustom.Stop()` called.
The request:
```
DELETE /stop?accountId=<ID>
```

Whenever the account does not exist respond with: 
```
400

2
```

## List
Your server must respond with a list of accounts to able to show all available users they can connect to.
```
GET /list

[
    {
        "accountId": "1",
        "addresses":
        [
            "192.168.1.50",
            "192.168.3.50",
            "26.525.151.51",
            "::1"
        ],
        "port": 35666
    },
        {
        "accountId": "2",
        "addresses":
        [
            "192.168.1.70",
            "192.168.3.95",
            "26.525.151.151",
            "::1"
        ],
        "port": 84643
    }
]
```
