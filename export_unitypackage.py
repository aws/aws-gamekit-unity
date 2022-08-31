# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0
"""
Run `python export_unitypackage.py --help` to learn how to use this program.
"""
import argparse
from fnmatch import fnmatch
import logging
import os
from pathlib import Path
import shutil
import subprocess
from typing import List, Dict


class Folders:
    # TempExportUnityPackage/
    TEMP_DIR = 'TempExportUnityPackage'

    # Assets/AWS GameKit/
    AWS_GAMEKIT = Path('Packages', 'com.amazonaws.gamekit')

    # Assets/AWS GameKit/Editor/
    EDITOR = Path(AWS_GAMEKIT, 'Editor')
    WINDOW_STATE = Path(EDITOR, 'WindowState')

    # Assets/AWS GameKit/Editor/CloudResources/
    CLOUD_RESOURCES = Path(EDITOR, 'CloudResources')
    BASE_TEMPLATES = Path(CLOUD_RESOURCES, '.BaseTemplates')
    INSTANCE_FILES = Path(CLOUD_RESOURCES, 'InstanceFiles')


class Files:
    README = 'README.md'
    OUTPUT_PACKAGE = '.'


def main():
    args = get_arguments()
    configure_logging(args.quiet)
    create_unitypackage(args.unity_application_full_path)


def get_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Export AWS GameKit to a tarball which can be imported to a Unity project through the Package Manager. May take several minutes.",
        epilog="Example Usage:\n"
               "(Windows) python export_unitypackage.py \"C:\\Program Files\\Unity\\Hub\\Editor\\2020.3.24f1\\Editor\\Unity.exe\"\n"
               "(Mac)     python export_unitypackage.py /Applications/Unity/Hub/Editor/2020.3.24f1/Unity.app/Contents/MacOS/Unity\n",
        formatter_class=argparse.RawTextHelpFormatter
    )
    parser.add_argument("unity_application_full_path", type=str, help="Full path to the Unity application installed on this computer. See 'Example Usage' below for details.")
    parser.add_argument("--quiet", action="store_true", default=False, help="If true, will only output error messages. Info messages will not be written to stdout.")

    args = parser.parse_args()
    if not Path(args.unity_application_full_path).is_file():
        raise ValueError(f'Unity does not exist at provided path: {args.unity_application_full_path}')

    return args


def configure_logging(quiet: bool):
    # Example of logging format:
    # INFO: Running command <> from directory <>
    logging.basicConfig(
        format='%(levelname)s: %(message)s',
        level=logging.ERROR if quiet else logging.INFO
    )


def create_unitypackage(unity_application_full_path: str):
    # Move all unwanted files to a temp directory so they don't get packaged up:
    unwanted_files = remove_unwanted_files()

    # Create the .unitypackage file:
    logging.info(f'Creating "{Files.OUTPUT_PACKAGE}"')
    logging.info('The following command may take several minutes to complete:')
    run_command([unity_application_full_path, '-batchmode', '-nographics', '-quit', '-projectPath', '.', '-executeMethod', 'Internal.Editor.PackageBuilder.ExportPackage', Files.OUTPUT_PACKAGE])

    # Restore unwanted files:
    unwanted_files.restore_all_files()


class TempDirectory:
    def __init__(self, temp_dir: Path):
        self.temp_dir = temp_dir
        self.original_path_to_temp_path_map: Dict[Path, Path] = {}

    def clear(self):
        delete_directory_tree(self.temp_dir)
        self.original_path_to_temp_path_map.clear()

    def move_to_temp(self, file_or_folder_to_move: Path):
        if not file_or_folder_to_move.exists():
            return

        if file_or_folder_to_move.is_absolute():
            raise ValueError(f'Absolute path given. Only accepts relative paths. Provided path: {file_or_folder_to_move}')

        destination_path = self.temp_dir.joinpath(file_or_folder_to_move)
        if not destination_path.parent.is_dir():
            destination_path.parent.mkdir(parents=True)

        file_or_folder_to_move.replace(destination_path)
        self.original_path_to_temp_path_map[file_or_folder_to_move] = destination_path

    def restore_all_files(self):
        for original_path, temp_path in self.original_path_to_temp_path_map.items():
            temp_path.replace(original_path)

        self.clear()


def remove_unwanted_files() -> TempDirectory:
    """Move all files that should not be packaged in the .unitypackage into a temp folder."""
    temp_dir = TempDirectory(get_temp_dir_path())
    temp_dir.clear()

    unwanted_window_state_files = get_all_files_in_folder_except_readme(Folders.WINDOW_STATE)
    for file in unwanted_window_state_files:
        temp_dir.move_to_temp(file)

    unwanted_instance_files = get_all_files_in_folder_except_readme(Folders.INSTANCE_FILES)
    for file in unwanted_instance_files:
        temp_dir.move_to_temp(file)

    unwanted_base_files = get_unwanted_base_template_files()
    for file in unwanted_base_files:
        temp_dir.move_to_temp(file)

    return temp_dir


def get_all_files_in_folder_except_readme(folder: Path) -> List[Path]:
    # Get all files in the folder except the README.md and README.md.meta:
    return [path for path in folder.iterdir() if not fnmatch(path, f'{Path(folder, Files.README)}*')]


def get_unwanted_base_template_files() -> List[Path]:
    """
    Get all files ignored by the BaseTemplates/ .gitignore, except for .meta files corresponding to non-ignored files.
    """
    # Store the root of the plugin that we should return to after handling base template files
    plugin_root = os.getcwd()

    # Change directory to the submodule so Git commands will run for the submodule:
    os.chdir(Folders.BASE_TEMPLATES)

    # Get files ignored by Git:
    unwanted_files = get_git_ignored_files_except_meta_files()

    # Add extra unwanted files:
    target_files = ['.crux_template.md', 'Config', 'Config.meta', 'CODE_OF_CONDUCT.md', 'CONTRIBUTING.md']
    for f in target_files:
        unwanted_files.append(Path(f))

    # Prepend the "full" relative path:
    unwanted_files_full_relative_path = [Folders.BASE_TEMPLATES.joinpath(file) for file in unwanted_files]

    os.chdir(plugin_root)

    return unwanted_files_full_relative_path


def get_git_ignored_files_except_meta_files() -> List[Path]:
    """
    Return all files ignored by Git in the current repository, except .meta files corresponding to non-ignored files.
    """
    ignored_files = get_git_ignored_files()

    meta_files = [file for file in ignored_files if file.suffix == '.meta']
    non_meta_files = [file for file in ignored_files if file.suffix != '.meta']
    unwanted_meta_files = [file for file in meta_files if file.parent.joinpath(file.stem) in non_meta_files]
    unwanted_files = unwanted_meta_files + non_meta_files

    return unwanted_files


def get_git_ignored_files() -> List[Path]:
    """Return all files being ignored by Git in the current repository."""

    # The following Git command returns a list looking like this:
    # "Would remove .idea/"
    # "Would remove cloudformation.meta"
    # ...
    #
    # The flags mean:
    # -n: name-only, don't actually delete
    # -d: include directories
    # -X: only files ignored by Git

    try:
        ignored_files_raw = run_command(['git', 'clean', '-ndX'], should_log_output=False)
    except subprocess.CalledProcessError:
        # The current working directory is not a repository
        return []
    ignored_files = [
        Path(line.replace('Would remove ', ''))
        for line in ignored_files_raw
    ]
    if Path('.') in ignored_files:
        ignored_files.remove(Path('.'))

    return ignored_files


def get_temp_dir_path() -> Path:
    return get_absolute_path_to_folder_containing_this_script().joinpath(Folders.TEMP_DIR)


def get_absolute_path_to_folder_containing_this_script() -> Path:
    return Path(os.path.dirname(os.path.abspath(__file__)))


def delete_directory_tree(directory: Path):
    if directory.is_dir():
        shutil.rmtree(str(directory))


def run_command(command: List[str], should_log_output: bool = True) -> List[str]:
    """Execute the command line, log it's output, and return it's output as a List[str]."""
    logging.info(f"Running command: {command} from directory: {os.getcwd()}")
    try:
        output = subprocess.check_output(command)
        decoded_output = output.decode('utf-8').split("\n")
        if should_log_output:
            for line in decoded_output:
                logging.info(line)
        return decoded_output
    except (subprocess.CalledProcessError, FileNotFoundError) as err:
        logging.error(f"Command: {command} from directory: {os.getcwd()} failed.")
        raise err


if __name__ == '__main__':
    main()
