# NFT Museum on WAX Blockchain

An open-source, Unity-based 3D NFT Museum that allows users to explore the assets of their WAX blockchain wallets in a visually immersive environment.

## Features

- **Immersive 3D Experience**: Navigate a fully custom-designed 3D museum built with Unity.
- **WAX Blockchain Integration**: Input your WAX wallet (or that of a friend) to view NFTs directly in the museum.
- **Collection Filtering**: Easily filter NFTs by collections (e.g., for your favorite game or PFP collection).
- **Open Source**: Fully customizable codebase, allowing developers to modify, extend, or adapt the project for personal or commercial use.
- **Educational Tool**: Use this repository as a starting point for your own project. Whether you want to showcase your NFT collection or build a Unity project with blockchain integration, the NFT Museum by Blacklusion is a great starting point.

## Motivation

The WAX blockchain has positioned itself as a gaming-focused blockchain, with big Web3 titles such as Alien Worlds utilizing WAX blockchain and Atomic Assets technologies. This sparks interest in creating Unity-based applications with WAX blockchain integration. This repository aims to reduce the barrier of entry for developers looking to create their own Unity-based applications with WAX integration by providing an example implementation of a lightweight blockchain integration.

## Getting Started

### Prerequisites

- [Unity](https://unity.com/) (version 2022.3.11f1 or newer)
- Basic understanding of Unity and blockchain technology
- (Optional): Any compatible WAX wallet, e.g., [WAX Cloud Wallet](https://wallet.wax.io/), to view your own assets.

### Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/Blacklusion/WAX-Blockchain-NFT-Museum
    cd WAX-Blockchain-NFT-Museum
    ```

2. **Open the project in Unity:**

    Open Unity Hub, click on "Open," and select the cloned repository folder.

3. **Run the Project:**

    Press the play button in Unity to start the application.

## Usage

1. **Input a Wallet Address**: Upon starting the application, input a wallet address.
2. **Explore the Museum**: Use the controls to navigate the 3D museum and view the NFTs owned by the wallet.
3. **Filter by Collection**: Use the filtering options to display NFTs from specific collections.

## Customization

This project is highly customizable, allowing you to adapt it to your needs:

- **Change 3D Models**: Modify or replace the existing museum models with your own designs.
- **Expand Functionality**: Add new features such as interactive NFT details, social sharing, or VR support.
- **Integrate Other Blockchains**: Extend the project to work with other blockchains beyond WAX. Integrating other blockchains using the Atomic Assets Standard, such as EOS, can be easily achieved.

## Technical Details

- **Built with**: Unity 3D
- **Lightweight Blockchain Integration**: Not all applications require the functionality to sign blockchain transactions from the game. This lightweight blockchain integration demonstrates how to fetch assets from the chain without relying on additional SDKs or requiring wallet authentication.
- **Atomic Assets**: Integration of the de facto industry standard for NFTs on the WAX blockchain.
- **Image Retrieval**: NFT images are fetched via IPFS, resized, and re-encoded.
- **Custom 3D Models**: All 3D models are custom-made and can be modified within Unity.
- **Skybox**: Adapt the atmosphere of the space by changing the Skybox of the museum. For example, generate custom Skyboxes from an AI prompt at [Skybox Blockade Labs](https://skybox.blockadelabs.com/).

## Contributing

Contributions are welcome! Whether it's reporting bugs, suggesting features, or submitting pull requests, your involvement is appreciated.

## Funded by WAX Labs
This project was funded by WAX Labs, an initiative by the WAX blockchain that provides grants to innovative projects aimed at expanding and enhancing the WAX ecosystem. WAX Labs supports developers, creators, and entrepreneurs who are building tools, applications, and experiences that contribute to the growth and adoption of the WAX blockchain, fostering a thriving Web3 community.

## License

This project is licensed under the MIT License. See the [LICENSE](/LICENSE) file for details.

---

Feel free to reach out if you have any questions or need assistance with using this project.
