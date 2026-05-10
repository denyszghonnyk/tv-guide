{
  pkgs ? import <nixpkgs> { },
}:

pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnet-sdk_9 # или та версия, которую вы используете
    fontconfig
    freetype
    xorg.libX11
    xorg.libICE
    xorg.libSM
  ];

  shellHook = ''
    export LD_LIBRARY_PATH="${
      pkgs.lib.makeLibraryPath (
        with pkgs;
        [
          fontconfig
          freetype
          xorg.libX11
          xorg.libICE
          xorg.libSM
        ]
      )
    }:''${LD_LIBRARY_PATH:-}"
  '';
}
