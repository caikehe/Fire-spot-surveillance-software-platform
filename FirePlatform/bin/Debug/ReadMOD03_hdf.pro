pro ReadMOD03_hdf,dir,seamask=seamask,SolarZenith=SolarZenith,SolarAzimuth=SolarAzimuth
 ; Set some constants
 seamask_name = "Land/SeaMask"
 sz_name = "SolarZenith"
 sa_name = "SolarAzimuth"
 
 ; Open the file and initialize the SD interface
 sd_id = HDF_SD_START( dir, /read )
 ; Find the index of the sds to read using its name
 seamask_sds_index = HDF_SD_NAMETOINDEX(sd_id,seamask_name)
 sz_index = HDF_SD_NAMETOINDEX(sd_id,sz_name)
 sa_index = HDF_SD_NAMETOINDEX(sd_id,sa_name)
 ; Select it
 seamask_sds_id = HDF_SD_SELECT( sd_id, seamask_sds_index )
 sz_id = HDF_SD_SELECT( sd_id, sz_index )
 sa_id = HDF_SD_SELECT( sd_id, sa_index )
 
 ; Read the data : you can notice that here, it is not needed to allocate the data array yourself
 HDF_SD_GETDATA, seamask_sds_id, seamask
 HDF_SD_GETDATA, sz_id, SolarZenith
 HDF_SD_GETDATA, sa_id, SolarAzimuth

 ; end access to SDS
 HDF_SD_ENDACCESS, seamask_sds_id
 HDF_SD_ENDACCESS, sz_id
 HDF_SD_ENDACCESS, sa_id
 ; close the hdf file
 HDF_SD_END, sd_id

 end