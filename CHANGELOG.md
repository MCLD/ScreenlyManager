# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/).

## [1.0.1] - 2019-04-23
### Changed
- Set the API request timeout to 30 seconds (down from the default of 100).

### Fixed
- Don't abort all hosts if one Screenly fails.

## [1.0.0] - 2019-04-23
### Added
- Uses [Screenly OSE API](http://ose.demo.screenlyapp.com/api/docs/) `v1` to support most installations.
- List all of the expiration date and names of slides on a particular installation.
- List all of the expiration date and names of slides older than a certain number of days.
- Remove all slides older than a certain number of days.
- Support using the [password protection feature via HTTP Basic authentication](https://github.com/Screenly/screenly-ose/blob/master/docs/http-basic-authentication.md) (note: not encrypted if you aren't using TLS!)
- Can be provided with a list of IP addresses or hostnames or a single IP address or hostname.
- Settings can be specified on the commandline or via environment variables (so it can be run in [Docker](https://docker.com/).

[1.0.1]: https://github.com/mcld/greatreadingadventure/tree/v1.0.1
[1.0.0]: https://github.com/mcld/greatreadingadventure/tree/v1.0.0
