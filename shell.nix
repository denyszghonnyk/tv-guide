{ pkgs ? import <nixpkgs> {} }:

let
  deps = with pkgs; [
    fontconfig
    icu
    xorg.libX11
    xorg.libICE
    xorg.libSM
    libGL
  ];
in
pkgs.mkShell {
  buildInputs = deps;

  shellHook = ''
    export LD_LIBRARY_PATH="${pkgs.lib.makeLibraryPath deps}:$LD_LIBRARY_PATH"
    echo "NixOS Graphics Environment Ready!"
  '';
}