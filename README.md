# B4
Our internal project bootstrapper.

## Disclaimer
B4 is by no means the most optimized battle-ready code, nor is it meant to be. It is a finite set of functionality designed to bootstrap a projects workspace setup.

## Steps
### K9
This acquires, installs, and updates locally the [K9](https://github.com/dotBunny/K9) set of utilities.
### K9Config
### Remote Packages
Allows for automatic checkout of repositories (remote packages) via a JSON definition, and subsequently adds or updates the local repository into a Unity projects manifest.

An example JSON definition file might look like:
```
{
    "items":[
        {
            "id": "com.dotbunny.gdx",
            "type": "Git",
            "uri": "git@github.com:dotBunny/GDX.git",
            "branch": "dev",
            "path": "com.dotbunny.gdx"
        },   
        {
            "id": "com.sample.tests",
            "type": "Git",
            "uri": "git@github.com:Sample/SampleProject.git",
            "branch": "release/special",
            "path": "custom-folder",
            "commit": "abd0fwd67e4797987d8ffs1d5171edf8d0d08510",
            "submodules": [
                "MySubModule"
            ],
            "mappings": {
                "com.sample.test": "Packages/com.sample.test",
                "com.sample.test2": "MySubModule/Packages/com.sample.test2",
            }
        }
    ]
}
```
### Find Unity
### Launch Unity


## License
B4 is licensed under the [BSL-1.0 License](https://choosealicense.com/licenses/bsl-1.0/).
> A copy of this license can be found at the root of the project in the `LICENSE` file.