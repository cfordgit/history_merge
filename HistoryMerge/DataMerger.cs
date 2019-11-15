using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.Text;

namespace HistoryMerge
{
    public class DataMerger
    {
        private static readonly String UPDATE_COMMAND = "UPDATE merged_documents SET source = (source || ',{0}'), instrument_book_page = (instrument_book_page || '|{1}') WHERE record_id = ~rowid~;";
        private static readonly String INSERT_COMMAND = "INSERT INTO merged_documents({0}) VALUES ({1});SELECT last_insert_rowid();";
        private static readonly String MERGED_DATA_COLUMNS = "source,entry_type,recording_date,instrument_book_page,document_type,transaction_type";

        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        private static NameValueCollection providers = new NameValueCollection() { { "ProviderA", "1" },{ "ProviderB", "2" } };       
        private SqlDataService dataService;
        private List<String> records = new List<String>();

        public DataMerger(IConfiguration config, ILogger logger)
        {
            this._config = config;
            this._logger = logger;
        }

        public Boolean MergeData()
        {
            SQLiteConnection con;
            SQLiteDataReader dr;
            dataService = new SqlDataService(_config, _logger);
            try
            {                
                using (con = dataService.CreateConnection())
                {
                    _logger.Log("Merging data...");
                    using (dr = dataService.ExecuteReader(con, "SELECT * FROM recording_documents ORDER BY entry_type, recording_date DESC"))
                    {
                        NameValueCollection lastRecord = null;
                        while (dr.Read())
                        {
                            NameValueCollection currentRecord = dr.GetValues();
                            ProcessRecord(currentRecord, lastRecord);
                            lastRecord = currentRecord;
                        }
                        _logger.Log("Data successfully merged");
                    }

                    CommitRecords();
                    CleanupData();
                    DisplayData(con);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log("Error processing data: " + ex.Message);
            }
            return false;
        }

        private void ProcessRecord(NameValueCollection currentRecord, NameValueCollection lastRecord)
        {
            Boolean isMerge = false;

            // Add record if we have a new date
            if (lastRecord == null || !lastRecord.Get("recording_date").Equals(currentRecord.Get("recording_date")))
            {
                isMerge = false;
            } else if (lastRecord.Get("entry_type").Equals(currentRecord.Get("entry_type")) && 
                CheckInstrumentNumbersMatch(lastRecord.Get("instrument_number"), currentRecord.Get("instrument_number"))) {
                isMerge = true;
            }

            records.Add(BuildCommand(currentRecord, isMerge));
        }

        private void CommitRecords()
        {
            _logger.Log("Committing records...");

            Object returnValue;
            long rowId = 0;
            records.ForEach(record => {
                returnValue = dataService.ExecuteScalar(record.Replace("~rowid~", rowId.ToString()));
                if (record.IndexOf("INSERT") != -1)
                {
                    rowId = (long) returnValue;
                }
            });
        }

        private void CleanupData()
        {
            _logger.Log("Performing cleanup...");

            // Cleanup duplicate source data
            dataService.ExecuteScalar("UPDATE merged_documents SET source = '1' WHERE source = '1,1'");
        }

        private void DisplayData(SQLiteConnection con)
        {
            _logger.Log("Displaying data...\n");

            SQLiteDataReader dr;
            StringBuilder sb = new StringBuilder();            
            using (dr = dataService.ExecuteReader(con, "SELECT * FROM merged_documents ORDER BY recording_date DESC"))
            {
                _logger.Log("record_id," + MERGED_DATA_COLUMNS);
                Object[] values = new Object[7];
                while (dr.Read())
                {
                    dr.GetValues(values);
                    sb.AppendJoin(",", values);
                    _logger.Log(sb.ToString());
                    sb.Clear();
                }
            }
        }

        private String BuildCommand(NameValueCollection currentRecord, Boolean isMerge)
        {
            String command;
            if (isMerge) {
                command = String.Format(UPDATE_COMMAND, providers[currentRecord.Get("provider")], FormatInstrumentNumber(currentRecord.Get("instrument_number")));
            } else {
                // source,entry_type,recording_date,instrument_book_page,document_type,transaction_type
                String[] fields = new string[6] { String.Format("\"{0}\"", providers[currentRecord.Get("provider")]),
                    String.Format("\"{0}\"", currentRecord.Get("entry_type")),
                    String.Format("\"{0}\"", currentRecord.Get("recording_date")),
                    String.Format("\"{0}\"", currentRecord.Get("instrument_number")),
                    String.Format("\"{0}\"", currentRecord.Get("document_type")),
                    String.Format("\"{0}\"", currentRecord.Get("transaction_type")) };
                
                command = String.Format(INSERT_COMMAND, MERGED_DATA_COLUMNS, String.Join(",", fields));
            }

            return command;
        }

        private Boolean CheckInstrumentNumbersMatch(String in1, String in2)
        {
            return FormatInstrumentNumber(in1) == FormatInstrumentNumber(in2);
        }

        private String FormatInstrumentNumber(String instrumentNumber)
        {
            // Remove trailing -00
            String formattedIN = (instrumentNumber.EndsWith("-00")) ? instrumentNumber[0..^3] : instrumentNumber;
            // Remove leading prefix xx- and ignore 0
            formattedIN = formattedIN.Substring(formattedIN.IndexOf("-") + 1).PadLeft(12, '0');
            return formattedIN;
        }
    }
}
