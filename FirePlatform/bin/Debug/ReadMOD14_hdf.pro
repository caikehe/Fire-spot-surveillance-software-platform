pro ReadMOD14_hdf,dir,Latitude_data=Latitude_data,Longitude_data=Longitude_data,line_data=line_data,sample_data=sample_data,Firemask_data=Firemask_data
 ; Set some constants
 Latitude_SDS_NAME="FP_latitude"
 Longitude_SDS_NAME="FP_longitude"
 Firemask = "fire mask"
 FP_line = "FP_line"
 FP_sample = "FP_sample"
 
 ; Open the file and initialize the SD interface
 sd_id = HDF_SD_START( dir, /read )
 ; Find the index of the sds to read using its name
 Latitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Latitude_SDS_NAME)
 Longitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Longitude_SDS_NAME)
 Firemask_index = HDF_SD_NAMETOINDEX(sd_id,Firemask)
 line_index = HDF_SD_NAMETOINDEX(sd_id,FP_line)
 sample_index = HDF_SD_NAMETOINDEX(sd_id,FP_sample)

 ; Select it
 Latitude_sds_id = HDF_SD_SELECT( sd_id, Latitude_sds_index )
 Longitude_sds_id = HDF_SD_SELECT( sd_id, Longitude_sds_index )
 Firemask_id = HDF_SD_SELECT( sd_id, Firemask_index)
 line_id = HDF_SD_SELECT( sd_id, line_index)
 sample_id = HDF_SD_SELECT( sd_id, sample_index)
 
 ; Set the data subset limits. Actually, it is read from the first element, so "start" is [0,0]. X_LENGTH elements will be read along the X axis and Y_LENGTH along Y.
 start=INTARR(2) ; the start position of the data to be read
 start[0] = 0
 start[1] = 0
 ; Read the data : you can notice that here, it is not needed to allocate the data array yourself
 HDF_SD_GETDATA, Latitude_sds_id, Latitude_data
 HDF_SD_GETDATA, Longitude_sds_id, Longitude_data
 HDF_SD_GETDATA, Firemask_id, Firemask_data
 HDF_SD_GETDATA, line_id, line_data
 HDF_SD_GETDATA, sample_id, sample_data
 
 ; end access to SDS
 HDF_SD_ENDACCESS, Latitude_sds_id
 HDF_SD_ENDACCESS, Longitude_sds_id
 HDF_SD_ENDACCESS, Firemask_id
 HDF_SD_ENDACCESS, line_id
 HDF_SD_ENDACCESS, sample_id
 ; close the hdf file
 HDF_SD_END, sd_id

   end