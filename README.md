# ScreenlyManager
ScreenlyManager is a cross-platform commandline program written using [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) to facilitate management of [Screenly OSE](https://www.screenly.io/ose/) installations.

**The latest release is [Version 1.0.1](https://github.com/mcld/ScreenlyManager/releases/latest).**

Current features:

- Uses [Screenly OSE API](http://ose.demo.screenlyapp.com/api/docs/) `v1` to support most installations.
- List all of the expiration date and names of slides on a particular installation.
- List all of the expiration date and names of slides older than a certain number of days.
- Remove all slides older than a certain number of days.
- Support using the [password protection feature via HTTP Basic authentication](https://github.com/Screenly/screenly-ose/blob/master/docs/http-basic-authentication.md) (note: not encrypted if you aren't using TLS!)
- Can be provided with a list of IP addresses or hostnames or a single IP address or hostname.
- Settings can be specified on the commandline or via environment variables (so it can be run in [Docker](https://docker.com/)).

## Use

### Command line
- `ScreenlyManager.exe -h` - Show help information.
- `ScreenlyManager.exe -v` - Show the current application version.
- `ScreenlyManager.exe -a:10.0.0.23` - List all assets on the Screenly OSE instance listening at 10.0.0.23.
- `ScreenlyManager.exe -a:10.0.0.23 -u:dan -p:koala` - List all assets using login "dan" and password "koala".
- `ScreenlyManager.exe -ls:30` - Prompt for IP addresses/host names and list slides that are 30 days or more expired.
- `ScreenlyManager.exe -a:10.0.0.23,10.0.0.42 -rm:30` - Remove all slides that are 30 days or more expired on the two listed instances.

### Environment variables
- `SCREENLY_ADDRESS` - same as the `-a` option: a single or comma-separated list of IP addresses or hostnames.
- `SCREENLY_API` - URL to the Screenly OSE API to use with `{0}` in place of the host name, defaults to `http://{0}/api/v1/assets`.
- `SCREENLY_USER` - same as the `-u` option: a username to use for password-protected installations.
- `SCREENLY_PASSWORD` - same as the `-p` option: a password to go with the username for password-protected installations.
- `SCREENLY_LIST` - same as the `-ls` option: an integer number of days to check for expired slides.
- `SCREENLY_REMOVE` - same as the `-rm` option: an integer number of days to remove expired slides.

### Docker
If you already have a server environment running Docker, it may be easier to use ScreenlyManager through a [Docker image](https://cloud.docker.com/u/mcld/repository/docker/mcld/screenlymanager). Here are some sample commands:

- `docker run --rm -e SCREENLY_ADDRESS=10.0.0.23,10.0.0.42 mcld/screenlymanager` - List all assets on the Screenly OSE instances listening at 10.0.0.23 and 10.0.0.42.
- `docker run --rm -e SCREENLY_ADDRESS=10.0.0.23 -e SCREENLY_REMOVE=30 mcld/screenlymanager` - Remove all slides that are 30 days or more expired on the Screenly OSE instance listening at 10.0.0.23.
- `docker run --rm --env-file screenly.env mcld/screenlymanager` - Run ScreenlyManager using the configuration specified in the `screenly.env` [Docker environment file](https://docs.docker.com/engine/reference/commandline/#set-environment-variables--e---env---env-file).
- `docker rmi mcld/screenlymanager:latest && docker pull mcld/screenlymanager:latest` - Upgrade to the latest Docker image of ScreenlyManager.

## License
ScreenlyManager source code is Copyright 2019 by the [Maricopa County Library District](https://mcldaz.org/) and is distributed under [The MIT License](http://opensource.org/licenses/MIT).
