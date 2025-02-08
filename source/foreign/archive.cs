```csharp
// vips__archive_free
void VipsArchiveFree(VipsArchive archive)
{
    // flush any pending writes to zip output
    if (archive.Archive != null)
        ArchiveWriteClose(archive.Archive);

    GString.Free(archive.BaseDirname);
    ArchiveWriteFree(ref archive.Archive);
    GObject.Free(archive);
}

// vips__archive_new_to_dir
VipsArchive VipsArchiveNewToDir(string baseDirname)
{
    // write to a filesystem directory
    VipsArchive archive = new VipsArchive();
    archive.BaseDirname = GString.New(baseDirname);

    return archive;
}

// vips__archive_new_to_target
VipsArchive VipsArchiveNewToTarget(VipsTarget target, string baseDirname, int compression)
{
    // write a zip to a target
    VipsArchive archive = new VipsArchive();
    archive.Target = target;
    archive.BaseDirname = GString.New(baseDirname);

    if (archive.Archive == null)
        archive.Archive = ArchiveWriteNew();

    // Set format to zip.
    if (!ArchiveWriteSetFormat(archive.Archive, ARCHIVE_FORMAT_ZIP))
    {
        VipsError("archive", "%s", _("unable to set zip format"));
        VipsArchiveFree(archive);
        return null;
    }

    // Remap compression=-1 to compression=6.
    if (compression == -1)
        compression = 6; /* Z_DEFAULT_COMPRESSION */

#if ARCHIVE_VERSION_NUMBER >= 3002000
    // Deflate compression requires libarchive >= v3.2.0.
    char[] compressionString = new char[2];
    compressionString[0] = '0' + compression;
    compressionString[1] = '\0';
    if (!ArchiveWriteSetFormatOption(archive.Archive, "zip", "compression-level", compressionString))
    {
        VipsError("archive", "%s", _("unable to set compression"));
        VipsArchiveFree(archive);
        return null;
    }
#else
    if (compression > 0)
        GWarning("libarchive >= v3.2.0 required for Deflate compression");
#endif

    // Do not pad last block.
    if (!ArchiveWriteSetBytesInLastBlock(archive.Archive, 1))
    {
        VipsError("archive", "%s", _("unable to set padding"));
        VipsArchiveFree(archive);
        return null;
    }

    // Register target callback functions.
    if (!ArchiveWriteOpen(archive.Archive, archive, null, zipWriteTargetCb, zipCloseTargetCb))
    {
        VipsError("archive", "%s", _("unable to open for write"));
        VipsArchiveFree(archive);
        return null;
    }

    return archive;
}

// vips__archive_mkdir
int VipsArchiveMkdir(VipsArchive archive, string dirname)
{
    // The ZIP format maintains a hierarchical structure, avoiding the need to create individual entries for each (sub-)directory.
    if (archive.Archive != null)
        return 0;

    return VipsArchiveMkdirFile(archive, dirname);
}

// vips__archive_mkdir_file
int VipsArchiveMkdirFile(VipsArchive archive, string dirname)
{
    // write to a filesystem directory
    char[] path = GBuildFilename(archive.BaseDirname, dirname, null);

    if (GDirCreateWithParents(path, 0777) && errno != EEXIST)
    {
        int save_errno = errno;
        string utf8name;

        utf8name = GFilenameDisplayName(path);
        VipsError("archive", _("unable to create directory \"%s\", %s"), utf8name, GStrerror(save_errno));

        GFree(utf8name);
        GFree(path);

        return -1;
    }

    GFree(path);

    return 0;
}

// vips__archive_mkfile_zip
int VipsArchiveMkFileZip(VipsArchive archive, string filename, byte[] buf, size_t len)
{
    // write a zip to a target
    struct ArchiveEntry entry;

    if (VipsWorkerLock(vips_libarchive_mutex))
        return -1;

    if (!(entry = ArchiveEntryNew()))
    {
        VipsError("archive", "%s", _("unable to create entry"));
        GMutexUnlock(vips_libarchive_mutex);
        return -1;
    }

    char[] path = GBuildFilename(archive.BaseDirname, filename, null);

    ArchiveEntrySetPathname(entry, path);
    ArchiveEntrySetMode(entry, S_IFREG | 0664);
    ArchiveEntrySetSize(entry, len);

    GFree(path);

    if (ArchiveWriteHeader(archive.Archive, ref entry))
    {
        VipsError("archive", "%s", _("unable to write header"));
        ArchiveEntryFree(ref entry);
        GMutexUnlock(vips_libarchive_mutex);
        return -1;
    }

    ArchiveEntryFree(ref entry);

    if (ArchiveWriteData(archive.Archive, buf, len) != len)
    {
        VipsError("archive", "%s", _("unable to write data"));
        GMutexUnlock(vips_libarchive_mutex);
        return -1;
    }

    GMutexUnlock(vips_libarchive_mutex);

    return 0;
}

// vips__archive_mkfile_file
int VipsArchiveMkFileFile(VipsArchive archive, string filename, byte[] buf, size_t len)
{
    // write to a filesystem directory
    char[] path = GBuildFilename(archive.BaseDirname, filename, null);
    FILE f;

    if (!(f = VipsFileOpenWrite(path, false)))
    {
        GFree(path);
        return -1;
    }

    if (VipsFileWrite(buf, sizeof(char), len, ref f))
    {
        GFree(path);
        fclose(f);
        return -1;
    }

    fclose(f);
    GFree(path);

    return 0;
}

// vips__archive_mkfile
int VipsArchiveMkFile(VipsArchive archive, string filename, byte[] buf, size_t len)
{
    // write a zip to a target
    return ((archive.Archive != null) ? VipsArchiveMkFileZip : VipsArchiveMkFileFile)(archive, filename, buf, len);
}
```