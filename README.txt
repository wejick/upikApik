Two of three components to build HMR peer to peer audio streaming
HMR stand for Hub, Muze and Red.
Hub is tracker which track and regulate files distribution between host.
Muze is music player component based on Bass library
Red is file streaming backend which is dealing with block schedulling method

This project Contain Muze and Red implementation; You can find Red implementation on RedToHub.cs and the rest is Muze. Red implementation is independent from Muze, however muze is poorly designed to use Red in not a good way.
