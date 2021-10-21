# LukeDictionary - DB Bootstrapper
This is a simple tool for bootstrapping a MongoDB database for LukeDictionary.

## Usage
Run with `--ConnectionString="<value>"` where <value> is your MongoDB connection string.  
Requires permissions to create databases, collections, indexes etc, so use connection string for user with dbAdmin role.

## Disclaimers
- Note that this tool assumes a MongoDB cluster is already up and running.
- Ensure that your IP Address is whitelisted in your MongoDB cluster.
- This tool might not get updated regularly. It's designed for internal use, primarily.
- This tool currently references entire LukeDictionary project to get access to the types. Until separate Core project is created (if it does at all), this tool will ignore settings files for configuration. Use cmd args instead.

## License
Copyright (c) 2021 DevSubmarine & TehGM

Based on tools for SnipLink by TehGM, and licensed under the same license.  
Licensed under [GNU General Public License v3.0](LICENSE) (GNU GPL-3.0).