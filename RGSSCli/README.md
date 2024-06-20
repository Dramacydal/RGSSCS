# RGSSCli

---

Provides a CLI tool to work with RPG Maker archives:
- extract files from archive
- list files inside archive
- create archive from directory
    

    RGSSCli.exe --help  
    Description:  
    RGSSCli: decrypts, encrypts, extracts various RPG Maker archives  

    Usage:
    RGSSCli [command] [options]  

    Options:  
    --version       Show version information  
    -?, -h, --help  Show help and usage information  

    Commands:  
    extract <archive>         Extracts files from archive  
    list <archive>            Lists files in archive  
    encrypt <path> <outfile>  Create archive from directory  

Following versions supported:
- File format 1 version:
  - RPG Maker XP *.rgssad
  - RPG Maker VX *.rgss2a
- File format 3 versions:
  - RPG Maker VX Ace *.rgss3a

---
