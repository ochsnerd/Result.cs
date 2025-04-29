{
  description = "Result";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  inputs.flake-utils.url = "github:numtide/flake-utils";
  inputs.flake-compat = {
    url = "github:edolstra/flake-compat";
    flake = false;
  };

  outputs = { self, nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };
      in {
        devShells.default = let
          tex = (pkgs.texlive.combine {
            inherit (pkgs.texlive)
              scheme-full
              dvisvgm
              dvipng
              wrapfig
              amsmath
              ulem
              hyperref
              beamer
              capt-of;
          });
        in pkgs.mkShell {
          buildInputs = with pkgs; [
            dotnet-sdk_9
            dotnet-runtime_9
            dotnetCorePackages.sdk_9_0
            dotnet-ef
          ];
          # https://stackoverflow.com/a/64370938
          DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 1;
        };
      });
}
