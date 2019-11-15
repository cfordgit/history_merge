
DROP TABLE IF EXISTS recording_documents;
DROP TABLE IF EXISTS merged_documents;
CREATE TABLE recording_documents(record_id INTEGER PRIMARY KEY, provider TEXT, entry_type TEXT, recording_date TEXT, doc_dated_date TEXT, instrument_number TEXT, transaction_type TEXT, document_type TEXT);
CREATE TABLE merged_documents(record_id INTEGER PRIMARY KEY, source TEXT, entry_type TEXT, recording_date TEXT, instrument_book_page TEXT, document_type TEXT, transaction_type TEXT);