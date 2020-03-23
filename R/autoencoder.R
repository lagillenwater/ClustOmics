library(reticulate)

#' Autoencoder function
#'
#' Reduce dimensionality using an Autoencoder
#' @param df dataframe to be reduced
#' @param layer.sizes
#' @param record.type ["csv", "npy", "npz", "tfrecord"] type of record format used for feeding autoencoder
#' @return 
#' @export

autoencoder <- function(df, hidden.layer.sizes = c(128, 64), embeddings.length = 16, 
                        pretrain.epochs = 100, finetune.epochs = 200,
                        pretrain.lr = 0.01, batch.size = 32, validation.split = 0.2,
                        use.gpu = FALSE, record.type = "tfrecord"){
  layer.sizes <- c(hidden.layer.sizes, embeddings.length)
  wd <- getwd()
  # TODO: prompt users for any system changes (can interfere with concurrently running python/R scripts)
  setwd("../inst/AE")
  pypath = Sys.getenv("PYTHONPATH")
  Sys.setenv(PYTHONPATH="C:/Users/easte/Documents/GitHub/ClustOmics/inst/AE")
  
  # TODO: Put in try catch to set wd even in event of failure
  converter.path <- sprintf("%scsv2tfrecord.py", "data/")
  record.converter <- source_python(converter.path)
  
  write.csv(df, file="data/tmp_out.csv")
  #reticulate::import("data/csv2tfrecord")
  # TODO: add support for other file types
  #if (record.type == "tfrecord"){
  csv2tfrecord(file_pattern="tmp_out.csv", data_dir="data")
  #}
	file.remove("data/tmp_out.csv")
	
	#setwd("model/")
  
  cmd.builder <- sprintf("python %s", "model/deepomicmodel.py")
  cmd.builder <- paste(cmd.builder, sprintf("--data_dir %s", "data"), sep=" ")
  cmd.builder <- paste(cmd.builder, sprintf("--input_dims %i", ncol(df)), sep=" ")
  cmd.builder <- paste(cmd.builder, sprintf("--layers %i", layer.sizes[1]), sep=" ")
  for (layer in layer.sizes[2:length(layer.sizes)]){
    cmd.builder <- paste(cmd.builder, sprintf(",%i", layer), sep="")
  }
  cmd.builder <- paste(cmd.builder, sprintf("--num_epochs %i", pretrain.epochs), sep=" ")
  cmd.builder <- paste(cmd.builder, sprintf("--num_comb_epochs %i", finetune.epochs), sep=" ")
  cmd.builder <- paste(cmd.builder, sprintf("--learn_rate %f", pretrain.lr), sep=" ")
  cmd.builder <- paste(cmd.builder, sprintf("--batch_size %i", batch.size), sep=" ")
  cmd.builder <- paste(cmd.builder, "--no_timestamp", sep=" ")
  system(cmd.builder)
  
  out_df <- read.csv("ae_out.csv")
  
  setwd(wd)
  Sys.setenv(PYTHONPATH=pypath)
  
  return(out_df)
}

#' Check Dependency function
#'
#' Check for all dependencies required by the autoencoder. Optionally, enable
#' installation of missing dependencies.
#' @param py.path path to preferred Python installation or environment (defaults to system install)
#' @param py.env ["python", "virtualenv", "conda"] type of python environment to run, defaults to standard python
#' @return TRUE if all required installations detected, FALSE otherwise
#' @export

check_dependencies <- function(py.path, py.env="python") {
  ae.installed <- F
  python.version <- 0
  tensorflow.version <- 0
  tf.gpu <- F

  # Check for Autoencoder installation
  # TODO: check for installation
  if (file.exists("../inst/AE")){
    print("Autoencoder found!")
  } else {
    cmd <- readline("This functionality requires installing an outside package from GitHub 
          (url: https://github.com/eastene/DeepOmic), continue [y/n]?")
    if (cmd == "y"){
      git2r::clone(url="https://github.com/eastene/DeepOmic.git", local_path = "../inst/AE/")
    }
  }
  
  switch(py.env,
    "python" = use_python(py.path, required = TRUE),
    "virtualenv" = use_virtualenv(py.path, required = TRUE),
    "conda" = use_condaenv(py.path, required = TRUE),
    stop(sprintf("Python executable or environment not found at: %s", py.path))
  )
  
  py.config <- py_config()

  # Check Python version
  if (py.config$version[1] == 2 || !py_available()){
    # Python version is less than 3.0 or none detected
    cmd <- readline("No exsiting installation of Python 3 detected, install now [y/n]?")
    if (cmd == "y"){
      stop("Function not implemented yet")
    }
    
  } else {
    print("Supported Python installation found!")
  }
  
  # Check TF version
  tf_import <- tryCatch(
    {
      import("tensorflow")
      print("Supported Tensorflow install found!")
    },
    # TF not installed
    error = function(cond) {
      cmd <- readline("No existing intallation of Tensorflow detected, install now [y/n]?")
      if (cmd == "y"){
        gpu <- readline("Install for GPU usage? (NOTE: requires CUDA and CUDNN install and one or more GPUs!) [y/n]?")
        if (gpu == "y"){
          py_install("tensorflow-gpu")
        } else {
          py_install("tensorflow")
        }
      } else{
          stop("Tensorflow installation not found. Autoencoder unavailable.")
      }
      return(NA)
    }
  )
  
  required.packages <- c("pandas", "matplotlib")
  imported.packages <- c("")
  misc_import <- tryCatch(
    {
      tmp.packages <- required.packages
      for (package in tmp.packages){
        import(package)
        required.packages <- required.packages[2:length(required.packages)]
      }
    },
    error = function(cond){
      print("One or more python packages not found")
      print("Packages not imported: (note: some may already be installed, see error message for more detail)")
      for (package in required.packages){
        print(package)
      }
      stop(cond)
    }
  )
  
  print("Autoencoder is available for use (see ?autoencoder for help)")
}
